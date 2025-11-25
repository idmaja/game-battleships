export const GameSetup = ({ 
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
        <div className="flex items-center justify-center min-h-screen p-8 bg-gray-100">
            <div className="w-full max-w-md p-8 bg-white rounded-lg shadow-lg">
                <h1 className="mb-6 text-3xl font-bold text-center">Initialize Game</h1>
                <form onSubmit={handleInitGame} className="space-y-4">
                    <div>
                        <label className="block mb-1 text-sm font-medium">Player Name</label>
                        <input type="text" value={playerName} onChange={(e) => setPlayerName(e.target.value)} required className="w-full px-3 py-2 border border-gray-300 rounded shadow-md" />
                    </div>
                    <div>
                        <label className="block mb-1 text-sm font-medium">Computer Name</label>
                        <input type="text" value={computerName} onChange={(e) => setComputerName(e.target.value)} required className="w-full px-3 py-2 border border-gray-300 rounded shadow-md" />
                    </div>
                    <div className="flex justify-center w-auto gap-2">
                        <div className="w-full">
                            <label className="block mb-1 text-sm font-medium">Board Width</label>
                            <input type="number" value={boardWidth} onChange={(e) => setBoardWidth(e.target.value)} required min="5" max="20" className="w-full px-3 py-2 border border-gray-300 rounded shadow-md" />
                        </div>
                        <div className="w-full">
                            <label className="block mb-1 text-sm font-medium">Board Height</label>
                            <input type="number" value={boardHeight} onChange={(e) => setBoardHeight(e.target.value)} required min="5" max="20" className="w-full px-3 py-2 border border-gray-300 rounded shadow-md" />
                        </div>
                    </div>
                    <div>
                        <label className="block mb-1 text-sm font-medium">Ship Lengths (comma separated)</label>
                        <input type="text" value={shipLengths} onChange={(e) => setShipLengths(e.target.value)} required className="w-full px-3 py-2 border border-gray-300 rounded shadow-md" />
                    </div>
                    <button type="submit" className="w-full py-2 text-white bg-blue-500 rounded hover:bg-blue-600 shadow-md transition-all">Start Game</button>
                </form>
            </div>
        </div>
    );
};
