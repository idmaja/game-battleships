import { ReactComponent as GameLogo } from '../assets/game-logo.svg';
import ship3 from '../assets/ship-3.svg';
import ship4 from '../assets/ship-4.svg';
import ship5 from '../assets/ship-5.svg';

export const GameSetup = ({
    loading, 
    playerName, 
    setPlayerName, 
    computerName, 
    setComputerName, 
    boardWidth, 
    setBoardWidth, 
    boardHeight, 
    setBoardHeight, 
    shipLengths, 
    setShipLengths, 
    handleInitGame 
}) => {
    return (
        <div className="flex items-center justify-center min-h-screen bg-gradient-to-br from-blue-50 to-blue-100">
            <div className="w-full max-w-lg p-10 bg-white border border-gray-100 shadow-xl rounded-2xl">
                <div className="mb-8 text-center">
                    <h1 className="flex justify-center mb-2 text-4xl font-bold text-gray-800">
                        <GameLogo/>
                    </h1>
                    <p className="text-sm text-gray-500">Configure your game settings</p>
                </div>
                
                <form onSubmit={handleInitGame} className="space-y-5">
                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className="block mb-2 text-sm font-semibold text-gray-700">Player Name</label>
                            <input 
                                type="text" 
                                value={playerName} 
                                onChange={(e) => setPlayerName(e.target.value)} 
                                required 
                                minLength = "3"
                                className="w-full px-4 py-2.5 border-2 border-gray-200 rounded-lg focus:border-blue-400 focus:outline-none transition-colors" 
                                placeholder="Your name"
                            />
                        </div>
                        <div>
                            <label className="block mb-2 text-sm font-semibold text-gray-700">Computer Name</label>
                            <input 
                                type="text" 
                                value={computerName} 
                                onChange={(e) => setComputerName(e.target.value)} 
                                required
                                minLength = "3" 
                                className="w-full px-4 py-2.5 border-2 border-gray-200 rounded-lg focus:border-blue-400 focus:outline-none transition-colors" 
                                placeholder="AI opponent"
                            />
                        </div>
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className="block mb-2 text-sm font-semibold text-gray-700">Width</label>
                            <input 
                                type="number" 
                                value={boardWidth} 
                                onChange={(e) => setBoardWidth(e.target.value)} 
                                required 
                                min="3" 
                                max="20" 
                                className="w-full px-4 py-2.5 border-2 border-gray-200 rounded-lg focus:border-blue-400 focus:outline-none transition-colors" 
                            />
                        </div>
                        <div>
                            <label className="block mb-2 text-sm font-semibold text-gray-700">Height</label>
                            <input 
                                type="number" 
                                value={boardHeight} 
                                onChange={(e) => setBoardHeight(e.target.value)} 
                                required 
                                min="3" 
                                max="20" 
                                className="w-full px-4 py-2.5 border-2 border-gray-200 rounded-lg focus:border-blue-400 focus:outline-none transition-colors" 
                            />
                        </div>
                    </div>

                    <div>
                        <label className="block mb-2 text-sm font-semibold text-gray-700">Ship Configuration</label>
                        <div className="grid grid-cols-3 gap-4">
                            <div>
                                <label className="block mb-1 text-xs text-gray-600">Size 5</label>
                                <img src={ship5} alt="ship-5" className="object-contain w-full h-16 mb-2" />
                                <input 
                                    type="number" 
                                    value={shipLengths.split(',')[0]?.trim() || 0}
                                    onChange={(e) => {
                                        const parts = shipLengths.split(',').map(s => s.trim());
                                        parts[0] = e.target.value;
                                        setShipLengths(parts.join(', '));
                                    }}
                                    min="0" 
                                    max="5"
                                    className="w-full px-3 py-2 transition-colors border-2 border-gray-200 rounded-lg focus:border-blue-400 focus:outline-none" 
                                />
                            </div>
                            <div>
                                <label className="block mb-1 text-xs text-gray-600">Size 4</label>
                                <img src={ship4} alt="ship-4" className="object-contain w-full h-16 mb-2" />
                                <input 
                                    type="number" 
                                    value={shipLengths.split(',')[1]?.trim() || 0}
                                    onChange={(e) => {
                                        const parts = shipLengths.split(',').map(s => s.trim());
                                        parts[1] = e.target.value;
                                        setShipLengths(parts.join(', '));
                                    }}
                                    min="0" 
                                    max="5"
                                    className="w-full px-3 py-2 transition-colors border-2 border-gray-200 rounded-lg focus:border-blue-400 focus:outline-none" 
                                />
                            </div>
                            <div>
                                <label className="block mb-1 text-xs text-gray-600">Size 3</label>
                                <img src={ship3} alt="ship-3" className="object-contain w-full h-16 mb-2" />
                                <input 
                                    type="number" 
                                    value={shipLengths.split(',')[2]?.trim() || 0}
                                    onChange={(e) => {
                                        const parts = shipLengths.split(',').map(s => s.trim());
                                        parts[2] = e.target.value;
                                        setShipLengths(parts.join(', '));
                                    }}
                                    min="0" 
                                    max="5"
                                    className="w-full px-3 py-2 transition-colors border-2 border-gray-200 rounded-lg focus:border-blue-400 focus:outline-none" 
                                />
                            </div>
                        </div>
                        <p className="mt-1.5 text-xs text-gray-500">Number of ships for each size</p>
                    </div>

                    <button 
                        type="submit"
                        disabled={loading}
                        className="w-full py-2 mt-6 text-white font-semibold bg-blue-500 rounded-lg 
                        hover:bg-blue-600 active:scale-[0.98] transition-all shadow-md hover:shadow-lg 
                        flex items-center justify-center disabled:opacity-70"
                    >
                        {loading ? (
                            <span className="loader"></span>
                        ) : 'Start Game'}
                    </button>
                </form>
            </div>
        </div>
    );
};
