import { DraggableShip } from './DraggableShip';
import { ArrowsHorizontalIcon, ArrowsVerticalIcon } from '@phosphor-icons/react';

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
        <div className="p-6 mb-6 bg-white rounded-lg shadow-md">
            <div className={`flex flex-wrap items-center justify-center gap-3 ${(placedShips.length !== allShips.length) && 'py-5'} mb-6`}>
                {shipsToPlace.map((ship, idx) => (
                    <DraggableShip 
                        key={`${ship}-${idx}`} 
                        length={ship} 
                        index={idx} 
                        shipOrientation={shipOrientation} 
                    />
                ))}
            </div>
            
            <div className="flex items-center justify-center gap-4 mb-4">
                <button 
                    onClick={() => setShipOrientation(shipOrientation === 'horizontal' ? 'vertical' : 'horizontal')} 
                    className="px-6 py-2.5 text-sm font-semibold text-gray-700 bg-gray-100 border-2 border-gray-300 rounded-lg hover:bg-gray-200 hover:border-gray-400 transition-all"
                >
                    {shipOrientation === 'horizontal' ? (
                        <span className='flex justify-between flex-auto gap-3'>
                            <ArrowsHorizontalIcon size={20} weight="bold" /> 
                            Horizontal
                        </span> 
                    ) : (
                        <span className='flex justify-between flex-auto gap-3'>
                            <ArrowsVerticalIcon size={20} weight="bold" /> 
                            Vertical
                        </span> 
                    )}
                </button>
                <button 
                    onClick={handleReady} 
                    disabled={placedShips.length !== allShips.length}
                    className="px-6 py-2.5 text-sm font-semibold text-white bg-green-500 rounded-lg hover:bg-green-600 disabled:bg-gray-300 disabled:cursor-not-allowed transition-all shadow-sm hover:shadow-md"
                >
                    ✓ Ready ({placedShips.length}/{allShips.length})
                </button>
            </div>

            {placedShips.length > 0 && (
                <div className="pt-4 border-t border-gray-200">
                    <p className="mb-3 text-sm font-medium text-center text-gray-600">Placed Ships (click to remove)</p>
                    <div className="flex flex-wrap justify-center gap-2">
                        {placedShips.map((ship, idx) => (
                            <button 
                                key={idx} 
                                onClick={() => handleRemoveShip(ship.length)}
                                className="px-4 py-2 text-sm font-medium text-white transition-all bg-red-500 rounded-lg shadow-sm hover:bg-red-600 active:scale-95"
                            >
                                ✕ Ship ({ship.length})
                            </button>
                        ))}
                    </div>
                </div>
            )}
        </div>
    );
}
