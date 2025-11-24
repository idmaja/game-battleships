import { useState, useEffect, useMemo } from 'react';
import { initializeGame, placeShip, attack, getBoard, removeShip } from '../services/api';
import { DndContext } from '@dnd-kit/core';
import { GameSetup } from './GameSetup';
import { ShipPlacement } from './ShipPlacement';
import { GameBoard } from './GameBoard';
import { Modal } from './Modal';
import * as signalR from '@microsoft/signalr';

export const MainLayout = () => {
    const [gameState, setGameState] = useState('init');
    const [playerName, setPlayerName] = useState('');
    const [computerName, setComputerName] = useState('');
    const [boardWidth, setBoardWidth] = useState(10);
    const [boardHeight, setBoardHeight] = useState(10);
    const [shipLengths, setShipLengths] = useState('5, 4, 3');
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
    const [dragOverCell, setDragOverCell] = useState(null);
    const [modalOpen, setModalOpen] = useState(false);
    const [modalContent, setModalContent] = useState({ title: '', message: '' });

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
        const width = parseInt(boardWidth);
        const height = parseInt(boardHeight);
        
        // validasi panjang ships
        const maxShipLength = Math.max(...ships);
        const minDimension = Math.min(width, height);
        
        if (maxShipLength > minDimension) {
            setModalContent({
                title: 'Invalid Ship Length',
                message: `Ship length (${maxShipLength}) exceeds board dimensions! Maximum allowed: ${minDimension}`
            });
            setModalOpen(true);
            return;
        }
        
        const totalShipCells = ships.reduce((sum, len) => sum + len, 0);
        const totalBoardCells = width * height;
        
        if (totalShipCells > totalBoardCells * 0.5) {
            setModalContent({
                title: 'Too Many Ships',
                message: `Total ship cells (${totalShipCells}) is too large for board size (${totalBoardCells} cells). Reduce ship lengths or increase board size.`
            });
            setModalOpen(true);
            return;
        }
        
        try {
            await initializeGame({
                playerName,
                computerName,
                boardWidth: width,
                boardHeight: height,
                shipLengthsPlayer: ships,
                shipLengthsComputer: ships
            });
            setShipsToPlace(ships);
            setAllShips(ships);
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
        
        let endRow = row; let endCol = col;
        if (shipOrientation === 'horizontal') 
            endCol = col + draggedShip - 1;
        else 
            endRow = row + draggedShip - 1;
       
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
        } catch (error) {
            setMessage('Invalid placement');
        }
        setDraggedShip(null);
    };

    const handleRemoveShip = async ( shipLength ) => {
        try {
            await removeShip({ playerName, shipLength: shipLength });
            
            const newPlaced = placedShips.filter(s => s.length !== shipLength);

            setPlacedShips(newPlaced);
            setShipsToPlace([...shipsToPlace, shipLength]);

            await loadBoards();
        } catch (error) {
            setMessage('Error removing ship');
        }
    };

    const handleReady = () => {
        if (placedShips.length === allShips.length) setGameState('playing');
    };

    const handleCellClick = async (row, col, isPlayer) => {
        if (gameState === 'playing' && !isPlayer) {
            const coord = `${String.fromCharCode(65 + col)}${row + 1}`;
            try {
                const response = await attack({ coordinate: coord });
                
                setScores(response.data.scores || {});
                
                if (response.data.isGameOver) setGameState('gameover');
                
                loadBoards();
            } catch (error) {
                setMessage('Invalid attack');
            }
        }
    };

    const loadBoards = async () => {
        if (!playerName || !computerName) return;
        try {
            const playerRes = await getBoard({ playerName: playerName });
            const computerRes = await getBoard({ playerName: computerName });

            setPlayerBoard(playerRes.data.cells || []);
            setComputerBoard(computerRes.data.cells || []);
        } catch (error) {
            console.error('Error loading boards:', error);
        }
    };

    const handleDragOver = (event) => {
        if (event.over && draggedShip) {
            const [row, col] = event.over.id.split('-').map(Number);
            setDragOverCell({ row, col });
        } else 
            setDragOverCell(null);
    };

    const previewCells = useMemo(() => {
        if (!dragOverCell || !draggedShip) return [];
        const cells = [];
        for (let i = 0; i < draggedShip; i++) {
            if (shipOrientation === 'horizontal')
                cells.push({ row: dragOverCell.row, col: dragOverCell.col + i }) 
            else
                cells.push({ row: dragOverCell.row + i, col: dragOverCell.col });
        }
        return cells;
    }, [dragOverCell, draggedShip, shipOrientation]);



    if (gameState === 'init') {
        return (
            <>
                <GameSetup 
                    playerName={playerName}
                    setPlayerName={setPlayerName}
                    computerName={computerName}
                    setComputerName={setComputerName}
                    boardWidth={boardWidth}
                    setBoardWidth={setBoardWidth}
                    boardHeight={boardHeight}
                    setBoardHeight={setBoardHeight}
                    shipLengths={shipLengths}
                    setShipLengths={setShipLengths}
                    handleInitGame={handleInitGame}
                />
                <Modal 
                    isOpen={modalOpen}
                    onClose={() => setModalOpen(false)}
                    title={modalContent.title}
                    message={modalContent.message}
                />
            </>
        );
    }

    return (
        <DndContext 
            onDragStart={(e) => setDraggedShip(parseInt(e.active.id.split('-')[1]))} 
            onDragOver={handleDragOver}
            onDragEnd={(e) => { handleDragEnd(e); setDragOverCell(null); }}
        >
            <div className="min-h-screen p-8 bg-gray-100">
                <div className="mx-auto max-w-7xl">
                <h1 className="mb-4 text-3xl font-bold text-center">Battleships</h1>
                
                <div className="p-4 mb-4 bg-white rounded shadow">
                    <p className="font-semibold text-center">{message}</p>
                </div>

                {gameState === 'setup' && (
                    <ShipPlacement 
                        shipsToPlace={shipsToPlace}
                        shipOrientation={shipOrientation}
                        setShipOrientation={setShipOrientation}
                        placedShips={placedShips}
                        allShips={allShips}
                        handleReady={handleReady}
                        handleRemoveShip={handleRemoveShip}
                    />
                )}

                <div className="flex gap-4">
                    <GameBoard 
                        cells={computerBoard}
                        isPlayer={false}
                        name={computerName}
                        score={scores[computerName]}
                        boardWidth={boardWidth}
                        boardHeight={boardHeight}
                        gameState={gameState}
                        handleCellClick={handleCellClick}
                        previewCells={[]}
                    />
                    <GameBoard 
                        cells={playerBoard}
                        isPlayer={true}
                        name={playerName}
                        score={scores[playerName]}
                        boardWidth={boardWidth}
                        boardHeight={boardHeight}
                        gameState={gameState}
                        handleCellClick={handleCellClick}
                        previewCells={previewCells}
                    />
                </div>

                {gameState === 'gameover' && (
                    <div className="mt-4 text-center">
                    <button onClick={() => window.location.reload()} className="px-6 py-2 text-white bg-blue-500 rounded hover:bg-blue-600">New Game</button>
                    </div>
                )}
                </div>
            </div>
        </DndContext>
    );
};