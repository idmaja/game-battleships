import { useState, useEffect, useMemo } from 'react';
import { initializeGame, placeShip, attack, getBoard, removeShip } from '../services/api';
import { DndContext, MouseSensor, PointerSensor, useSensor, useSensors } from '@dnd-kit/core';
import { GameSetup } from './GameSetup';
import { ShipPlacement } from './ShipPlacement';
import { GameBoard } from './GameBoard';
import { Modal } from './Modal';
import * as signalR from '@microsoft/signalr';
import { SmileySadIcon, SwordIcon, TrophyIcon } from '@phosphor-icons/react';
import { ReactComponent as GameLogo } from '../assets/game-logo.svg';

export const MainLayout = () => {
    const [gameState, setGameState] = useState('init');
    const [playerName, setPlayerName] = useState('PLAYER');
    const [computerName, setComputerName] = useState('COMPUTER');
    const [boardWidth, setBoardWidth] = useState(8);
    const [boardHeight, setBoardHeight] = useState(8);
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
    const [loading, setLoading] = useState(false);
    const [modalContent, setModalContent] = useState({ type: '', title: '', message: '' });

    useEffect(() => {
        const newConnection = new signalR.HubConnectionBuilder()
            .withUrl('http://localhost:5069/gameHub') // private network
            // .withUrl('http://172.168.100.25:5069/gameHub') // public network
            .withAutomaticReconnect()
            .build();

        setConnection(newConnection);
    }, []);

    useEffect(() => {
        if (connection) {
            connection.start()
                .then(() => {
                    connection.on('ReceiveMessage', (receivedMessage) => {
                        if (receivedMessage.includes('Winner')) {

                            const messageParts = receivedMessage.split('|');

                            const parseData = str => {
                                const clean = str.split(':')[1].trim();        
                                const name = clean.split('(')[0].trim();          
                                const score = clean.split('(')[1].split(')')[0]; 
                                return { name, score };
                            };

                            const winnerData = parseData(messageParts[0]);
                            const loserData = parseData(messageParts[1]);

                            setModalContent({
                                type: 'game-over',
                                title: (
                                    <span className='flex justify-center'>
                                        Game Over!
                                    </span>
                                ),
                                message: (
                                    <span className="mt-2 space-y-2 animate-fade">
                                        <div className="flex items-center gap-2 text-xl">
                                            <span className="text-2xl font-bold text-gray-900">{winnerData.name}</span>
                                            <span className="font-bold text-blue-600">({winnerData.score})</span>
                                            <span className="font-semibold text-green-600">wins!</span>
                                            <TrophyIcon 
                                                size={32} 
                                                weight="fill" 
                                                className="text-yellow-400 p-1 drop-shadow-[0_0_10px_rgba(255,215,0,0.9)] animate-pulse"
                                            />
                                        </div>
                                        <div className="flex items-center gap-2 text-xl">
                                            <span className="text-2xl font-bold text-red-700">{loserData.name}</span>
                                            <span className="font-bold text-red-500">({loserData.score})</span>
                                            <span className="font-semibold text-red-600">loses.</span>
                                            <SmileySadIcon 
                                                size={32} 
                                                weight="fill"
                                                className="text-red-600 p-1 drop-shadow-[0_0_10px_rgba(255, 215, 0, 0.9)] animate-pulse" 
                                            />
                                        </div>
                                    </span>
                                )
                            });
                            setModalOpen(true);
                        } else {
                            const messageParts = receivedMessage.split('|');
                            const isHitLeft = messageParts[0].includes("hit")
                            const isHitRight = messageParts[1].includes("hit")
                            setMessage(
                                <>
                                     <div className="flex flex-col gap-2">
                                        <span className={`${isHitLeft ? 'text-red-600' : 'text-blue-600'} font-bold`}>{messageParts[0]}</span>
                                        <span className={`${isHitRight ? 'text-red-600' : 'text-blue-600'} font-bold`}>{messageParts[1]}</span>
                                    </div>
                                </>
                            );
                        }
                    });
                })
                .catch(err => console.error('SignalR connection error:', err));

            return () => {
                connection.stop();
            };
        }
    }, [connection]);

    useEffect(() => {
        if (gameState === 'gameover' && connection)
            connection.stop();
    }, [gameState, connection])

    // set timer message notif
    useEffect(() => {
        if (!message) return;

        const timer = setTimeout(() => {
            setMessage(null);
        }, 3000);

        return () => clearTimeout(timer);
    }, [message]);

    const sensors = useSensors(
        useSensor(PointerSensor, {
            activationConstraint: {
                distance: 5,
            },
        }),
        useSensor(MouseSensor, {
            activationConstraint: {
                distance: 5,
            },
        })
    );

    const handleInitGame = async (e) => {
        e.preventDefault();
        setLoading(true);

        const parts = shipLengths.split(',').map(s => s.trim());

        for (let i = 0; i < parts.length; i++) {
            const part = parts[i];

            // kosong antara koma
            if (part.length === 0) {
                setModalContent({
                    type: 'invalid-input',
                    title: 'Invalid Ship Input',
                    message: `You have an empty value near comma last number "${parts[i-1].toString()}". Check for double commas like ",," or a trailing comma.`
                });
                setModalOpen(true);
                return;
            }

            // bukan angka
            if (!/^\d+$/.test(part)) {
                const invalidChars = [...part].filter(c => !/\d/.test(c)).join(' ');
                setModalContent({
                    type: 'invalid-input',
                    title: 'Invalid Ship Input',
                    message: `The value "${part}" contains invalid characters: ${invalidChars}. Ship lengths must use digits only.`
                });
                setModalOpen(true);
                return;
            }

            // nol atau negatif
            const value = parseInt(part, 10);
            if (value <= 0) {
                setModalContent({
                    type: 'invalid-input',
                    title: 'Invalid Ship Input',
                    message: `Ship length "${part}" must be greater than zero.`
                });
                setModalOpen(true);
                return;
            }
        }

        const ships = parts.map(Number);
        const width = parseInt(boardWidth);
        const height = parseInt(boardHeight);
        
        // validasi panjang ships
        const maxShipLength = Math.max(...ships);
        const minDimension = Math.min(width, height);
        
        if (maxShipLength > minDimension) {
            setModalContent({
                type: 'inv-ship-length',
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
                type: 'too-many-ships',
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
            setModalContent({
                type: 'error-init-game',
                title: 'Error',
                message: `Failed to Initialize Game: ${error}`
            });
            setModalOpen(true);
        }

        setLoading(false);
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
        if (placedShips.length === allShips.length) {
            setGameState('playing');

            setModalContent({
                type: 'battle-start',
                title: (
                    <span className='flex justify-center'>                        
                        Game
                        <span className="flex justify-center gap-1">
                            <SwordIcon size={32} weight="fill" className="text-red-500 rotate-[-20deg]" />
                            <SwordIcon size={32} weight="fill" className="text-red-500 scale-x-[-1] rotate-[20deg]" />
                        </span>
                        Start!!
                    </span>
                ),
                message: (
                    <span className="flex text-xl font-semibold tracking-wide text-center">
                        Your command begins. Strike with precision.
                    </span>
                )
            });
            setModalOpen(true);
        }
    };

    const handleCellClick = async (row, col, isPlayer) => {
        if (gameState === 'playing' && !isPlayer) {
            const coord = `${String.fromCharCode(65 + col)}${row + 1}`;
            try {
                const response = await attack({ coordinate: coord });
                
                setScores(response.data.scores || {});
                
                if (response.data.isGameOver) setGameState('gameover');
                
                await loadBoards();
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
                    loading={loading}
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
                    type={modalContent.type}
                    title={modalContent.title}
                    message={modalContent.message}
                />
            </>
        );
    }

    const content = (
        <div className="min-h-screen p-8 bg-gradient-to-br from-gray-50 to-gray-100">
            <div className="mx-auto max-w-7xl">
                <div className="mb-6 text-center">
                    <div className='flex justify-center flex-auto gap-3'>
                        <div className='text-3xl'>
                            <GameLogo/>
                        </div>
                    </div>
                    <p className="mt-2 text-sm text-gray-500">
                        {gameState === 'setup' && 'Place your ships on the board'}
                        {gameState === 'playing' && 'Attack the enemy board!'}
                        {gameState === 'gameover' && 'Game Over'}
                    </p>
                </div>
                
                {message && (
                    <div className="max-w-2xl p-4 mx-auto mb-6 text-center bg-white border-l-4 border-blue-500 rounded-lg shadow-sm">
                        <div className="text-gray-700">{message}</div>
                    </div>
                )}

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

                <div className="flex flex-col gap-6 lg:flex-row">
                    <div className='flex-1 order-2 lg:order-1v'>
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
                            isWinner={
                                (gameState === 'playing' || gameState === 'gameover') 
                                && (scores[computerName] || 0) > (scores[playerName] || 0) 
                                ? true : (scores[computerName] || 0) < (scores[playerName] || 0) 
                                ? false : undefined
                            }
                        />
                    </div>
                    <div className='flex-1 order-1 lg:order-2'>
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
                            isWinner={
                                (gameState === 'playing' || gameState === 'gameover') 
                                && (scores[playerName] || 0) > (scores[computerName] || 0) 
                                ? true : (scores[playerName] || 0) < (scores[computerName] || 0) 
                                ? false : undefined
                            }
                        />
                    </div>
                </div>

                {gameState === 'gameover' ? (
                    <div className="mt-8 text-center">
                        <button 
                            onClick={() => window.location.reload()} 
                            className="px-8 py-3 text-white font-semibold bg-blue-500 rounded-lg hover:bg-blue-600 active:scale-[0.98] transition-all shadow-md hover:shadow-lg"
                        >
                            New Game
                        </button>
                    </div>
                ) : (
                    <div className="mt-8 text-center">
                        <button 
                            onClick={() => window.location.reload()}
                            className="px-8 py-3 text-white font-semibold bg-blue-500 rounded-lg hover:bg-blue-600 active:scale-[0.98] transition-all shadow-md hover:shadow-lg"
                        >
                            Reset Game
                        </button>
                    </div>
                )}
            </div>
        </div>
    );

    if (gameState === 'setup') {
        return (
            <>
                <DndContext 
                    sensors={sensors}
                    onDragStart={(e) => setDraggedShip(parseInt(e.active.id.split('-')[1]))} 
                    onDragOver={handleDragOver}
                    onDragEnd={(e) => { handleDragEnd(e); setDragOverCell(null); }}
                >
                    {content}
                </DndContext>
                <Modal 
                    isOpen={modalOpen}
                    onClose={() => setModalOpen(false)}
                    type={modalContent.type}
                    title={modalContent.title}
                    message={modalContent.message}
                />
            </>
        );
    }

    return (
        <>
            {content}
            <Modal 
                isOpen={modalOpen}
                onClose={() => setModalOpen(false)}
                type={modalContent.type}
                title={modalContent.title}
                message={modalContent.message}
            />
        </>
    );
};