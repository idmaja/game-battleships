import { DraggableShip } from './DraggableShip';

export const ShipPlacement = ({ 
    shipsToPlace, 
    shipOrientation, 
    setShipOrientation, 
    placedShips, 
    allShips, 
    handleReady, 
    handleRemoveShip 
}) => {
    return (
        <div className="p-4 mb-4 bg-white rounded shadow">
            <div className="flex flex-wrap items-center justify-center gap-2 mb-4">
                {shipsToPlace.map((ship, idx) => (
                    <DraggableShip 
                        key={`${ship}-${idx}`} 
                        length={ship} 
                        index={idx} 
                        shipOrientation={shipOrientation} 
                    />
                ))}
            </div>
            <div className="space-x-4 text-center">
                <button 
                    onClick={() => setShipOrientation(shipOrientation === 'horizontal' ? 'vertical' : 'horizontal')} 
                    className="px-4 py-2 text-white bg-blue-500 rounded"
                >
                    Orientation: {shipOrientation}
                </button>
                <button 
                    onClick={handleReady} 
                    disabled={placedShips.length !== allShips.length}
                    className="px-4 py-2 text-white bg-green-500 rounded disabled:bg-gray-400 disabled:cursor-not-allowed"
                >
                    Ready ({placedShips.length}/{allShips.length})
                </button>
            </div>
            {placedShips.length > 0 && (
                <div className="mt-4">
                    <p className="mb-2 text-sm text-center">Placed Ships (click to remove):</p>
                    <div className="flex flex-wrap justify-center gap-2">
                        {placedShips.map((ship, idx) => (
                            <button 
                                key={idx} 
                                onClick={() => handleRemoveShip(ship.length)}
                                className="px-3 py-1 text-sm text-white bg-red-500 rounded hover:bg-red-600"
                            >
                                Ship ({ship.length})
                            </button>
                        ))}
                    </div>
                </div>
            )}
        </div>
    );
};
