import { useState, useEffect } from 'react';
import { initializeGame, placeShip, attack, getBoard, removeShip } from '../services/api';
import { DndContext, useDraggable } from '@dnd-kit/core';
import { BoardCell } from './Board';
import * as signalR from '@microsoft/signalr';

export const MainLayout = () => {
    const [gameState, setGameState] = useState('init');
    const [playerName, setPlayerName] = useState('');
    const [computerName, setComputerName] = useState('');
    const [boardWidth, setBoardWidth] = useState(10);
    const [boardHeight, setBoardHeight] = useState(10);
    const [shipLengths, setShipLengths] = useState('5,4,3');
    const [playerBoard, setPlayerBoard] = useState([]);
    const [computerBoard, setComputerBoard] = useState([]);
    const [shipsToPlace, setShipsToPlace] = useState([]);
    const [allShips, setAllShips] = useState([]);
    const [placedShips, setPlacedShips] = useState([]);
    const [draggedShip, setDraggedShip] = useState(null);
    const [shipOrientation, setShipOrientation] = useState('horizontal');
    const [message, setMessage] = useState('');
    const [scores, setScores] = useState({});
    const [connection, setConnection] = useState(null);

    useEffect(() => {
        const newConnection = new signalR.HubConnectionBuilder()
            .withUrl('http://localhost:5069/gameHub')
            .withAutomaticReconnect()
            .build();

        setConnection(newConnection);
    }, []);

    useEffect(() => {
        if (connection) {
            connection.start()
                .then(() => {
                    connection.on('ReceiveMessage', (msg) => {
                        setMessage(msg);
                    });
                })
                .catch(err => console.error('SignalR connection error:', err));

            return () => {
                connection.stop();
            };
        }
    }, [connection]);

    const handleInitGame = async (e) => {
        e.preventDefault();
        const ships = shipLengths.split(',').map(s => parseInt(s.trim()));
        try {
            await initializeGame({
                playerName,
                computerName,
                boardWidth: parseInt(boardWidth),
                boardHeight: parseInt(boardHeight),
                shipLengthsPlayer: ships,
                shipLengthsComputer: ships
            });
            setShipsToPlace(ships);
            setAllShips(ships);
            // setMessage('Drag ships to board');
            setGameState('setup');
            await loadBoards();
        } catch (error) {
            setMessage('Error initializing game');
        }
    };

    const handleDragEnd = async (event) => {
        const { over, active } = event;
        if (!over || !draggedShip) return;

        const [row, col] = over.id.split('-').map(Number);
        const shipIndex = parseInt(active.id.split('-')[2]);
        const start = `${String.fromCharCode(65 + col)}${row + 1}`;
        
        let endRow = row;
        let endCol = col;
        if (shipOrientation === 'horizontal') {
            endCol = col + draggedShip - 1;
        } else {
            endRow = row + draggedShip - 1;
        }
        const end = `${String.fromCharCode(65 + endCol)}${endRow + 1}`;

        try {
            await placeShip({
                playerName,
                shipLength: draggedShip,
                start,
                end
            });
            
            const newShips = [...shipsToPlace];
            const indexToRemove = newShips.findIndex((s, i) => s === draggedShip && i === shipIndex);
            newShips.splice(indexToRemove, 1);
            setShipsToPlace(newShips);
            setPlacedShips([...placedShips, { length: draggedShip, index: shipIndex, start, end }]);
            await loadBoards();
            // setMessage(newShips.length === 0 ? 'All ships placed! Click Ready to start' : 'Drag ships to board');
        } catch (error) {
            setMessage('Invalid placement');
        }
        setDraggedShip(null);
    };

    const handleRemoveShip = async ( shipLength ) => {
        try {
            await removeShip({
                playerName,
                shipLength: shipLength,
            });
            
            const newPlaced = placedShips.filter(s => s.length !== shipLength);
            setPlacedShips(newPlaced);
            setShipsToPlace([...shipsToPlace, shipLength]);
            await loadBoards();
            // setMessage(`Ship (${shipLength}) removed. Place it again`);
        } catch (error) {
            setMessage('Error removing ship');
        }
    };

    const handleReady = () => {
        if (placedShips.length === allShips.length) {
            setGameState('playing');
            // setMessage('Attack enemy board');
        }
    };

    const handleCellClick = async (row, col, isPlayer) => {
        if (gameState === 'playing' && !isPlayer) {
            const coord = `${String.fromCharCode(65 + col)}${row + 1}`;
            try {
                const response = await attack({ coordinate: coord });
                const data = response.data;
                
                // setMessage(data.message + (data.computerHit ? ` | Computer hit` : ` | Computer missed`));
                setScores(data.scores || {});
                
                if (data.isGameOver) {
                    setGameState('gameover');
                    // setMessage('Game Over! ' + (data.scores?.[playerName] > data.scores?.[computerName] ? 'You Win!' : 'Computer Wins!'));
                }
                
                loadBoards();
            } catch (error) {
                setMessage('Invalid attack');
            }
        }
    };

    const loadBoards = async () => {
        if (!playerName || !computerName) return;
        try {
            const playerRes = await getBoard(playerName);
            const computerRes = await getBoard(computerName);
            setPlayerBoard(playerRes.data.cells || []);
            setComputerBoard(computerRes.data.cells || []);
        } catch (error) {
            console.error('Error loading boards:', error);
        }
    };

    const DraggableShip = ({ length, index }) => {
        const { attributes, listeners, setNodeRef, transform, isDragging } = useDraggable({ id: `ship-${length}-${index}` });
        const style = transform ? { transform: `translate3d(${transform.x}px, ${transform.y}px, 0)` } : undefined;
        
        const handleContextMenu = (e) => {
            e.preventDefault();
            if (isDragging) {
                setShipOrientation(shipOrientation === 'horizontal' ? 'vertical' : 'horizontal');
            }
        };
        
        return (
            <div 
                ref={setNodeRef} 
                style={style} 
                {...listeners} 
                {...attributes} 
                onContextMenu={handleContextMenu}
                className="cursor-move inline-block"
            >
                <div className={`flex gap-1 ${shipOrientation === 'vertical' ? 'flex-col' : 'flex-row'}`}>
                    {Array.from({ length }).map((_, i) => (
                        <div key={i} className="w-8 h-8 bg-gray-600 border border-white" />
                    ))}
                </div>
            </div>
        );
    };

    const renderBoard = (cells, isPlayer, name, score) => {
        const h = parseInt(boardHeight);
        const w = parseInt(boardWidth);
        const grid = Array.from({ length: h }, () => Array(w).fill(null));
        
        cells.forEach(cell => {
            grid[cell.row][cell.col] = cell;
        });

        return (
            <div className="flex-1 bg-white p-6 rounded shadow">
                <h2 className="text-2xl font-bold mb-2">{name}</h2>
                <p className="text-lg mb-4">Score: {score || 0}</p>
                <div className="grid gap-1" style={{gridTemplateColumns: `repeat(${w}, minmax(0, 1fr))`, maxWidth: `${w * 36}px`}}>
                {grid.map((row, rowIdx) => 
                    row.map((cell, colIdx) => (
                    <BoardCell key={`${rowIdx}-${colIdx}`} row={rowIdx} col={colIdx} cell={cell} isPlayer={isPlayer} gameState={gameState} handleCellClick={handleCellClick} />
                    ))
                )}
                </div>
            </div>
        );
    };

    if (gameState === 'init') {
        return (
            <div className="min-h-screen bg-gray-100 flex items-center justify-center p-8">
                <div className="bg-white p-8 rounded shadow max-w-md w-full">
                    <h1 className="text-3xl font-bold text-center mb-6">Initialize Game</h1>
                    <form onSubmit={handleInitGame} className="space-y-4">
                        <div>
                            <label className="block text-sm font-medium mb-1">Player Name</label>
                            <input type="text" value={playerName} onChange={(e) => setPlayerName(e.target.value)} required className="w-full border border-gray-300 rounded px-3 py-2" />
                        </div>
                        <div>
                            <label className="block text-sm font-medium mb-1">Computer Name</label>
                            <input type="text" value={computerName} onChange={(e) => setComputerName(e.target.value)} required className="w-full border border-gray-300 rounded px-3 py-2" />
                        </div>
                        <div>
                            <label className="block text-sm font-medium mb-1">Board Width</label>
                            <input type="number" value={boardWidth} onChange={(e) => setBoardWidth(e.target.value)} required min="5" max="20" className="w-full border border-gray-300 rounded px-3 py-2" />
                        </div>
                        <div>
                            <label className="block text-sm font-medium mb-1">Board Height</label>
                            <input type="number" value={boardHeight} onChange={(e) => setBoardHeight(e.target.value)} required min="5" max="20" className="w-full border border-gray-300 rounded px-3 py-2" />
                        </div>
                        <div>
                            <label className="block text-sm font-medium mb-1">Ship Lengths (comma separated)</label>
                            <input type="text" value={shipLengths} onChange={(e) => setShipLengths(e.target.value)} required className="w-full border border-gray-300 rounded px-3 py-2" />
                        </div>
                        <button type="submit" className="w-full bg-blue-500 text-white py-2 rounded hover:bg-blue-600">Start Game</button>
                    </form>
                </div>
            </div>
        );
    }

    return (
        <DndContext onDragStart={(e) => setDraggedShip(parseInt(e.active.id.split('-')[1]))} onDragEnd={handleDragEnd}>
            <div className="min-h-screen bg-gray-100 p-8">
                <div className="max-w-7xl mx-auto">
                <h1 className="text-3xl font-bold text-center mb-4">Battleships</h1>
                
                <div className="bg-white p-4 rounded shadow mb-4">
                    <p className="text-center font-semibold">{message}</p>
                </div>

                {gameState === 'setup' && (
                    <div className="bg-white p-4 rounded shadow mb-4">
                        <div className="flex justify-center gap-4 mb-4 flex-wrap">
                            {shipsToPlace.map((ship, idx) => <DraggableShip key={`${ship}-${idx}`} length={ship} index={idx} />)}
                        </div>
                        <div className="text-center space-x-4">
                            <button onClick={() => setShipOrientation(shipOrientation === 'horizontal' ? 'vertical' : 'horizontal')} className="bg-blue-500 text-white px-4 py-2 rounded">
                                Orientation: {shipOrientation}
                            </button>
                            <button 
                                onClick={handleReady} 
                                disabled={placedShips.length !== allShips.length}
                                className="bg-green-500 text-white px-4 py-2 rounded disabled:bg-gray-400 disabled:cursor-not-allowed"
                            >
                                Ready ({placedShips.length}/{allShips.length})
                            </button>
                        </div>
                        {placedShips.length > 0 && (
                            <div className="mt-4">
                                <p className="text-center text-sm mb-2">Placed Ships (click to remove):</p>
                                <div className="flex justify-center gap-2 flex-wrap">
                                    {placedShips.map((ship, idx) => (
                                        <button 
                                            key={idx} 
                                            onClick={() => handleRemoveShip(ship.length)}
                                            className="bg-red-500 text-white px-3 py-1 rounded text-sm hover:bg-red-600"
                                        >
                                            Ship ({ship.length})
                                        </button>
                                    ))}
                                </div>
                            </div>
                        )}
                    </div>
                )}

                <div className="flex gap-4">
                    {renderBoard(computerBoard, false, computerName, scores[computerName])}
                    {renderBoard(playerBoard, true, playerName, scores[playerName])}
                </div>

                {gameState === 'gameover' && (
                    <div className="text-center mt-4">
                    <button onClick={() => window.location.reload()} className="bg-blue-500 text-white px-6 py-2 rounded hover:bg-blue-600">New Game</button>
                    </div>
                )}
                </div>
            </div>
        </DndContext>
    );
};