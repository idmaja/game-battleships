import { BoardCell } from './BoardCell';
import { TrophyIcon, SmileySadIcon } from '@phosphor-icons/react';

export const GameBoard = ({ 
    cells, 
    isPlayer, 
    name, 
    score, 
    boardWidth, 
    boardHeight, 
    gameState, 
    handleCellClick, 
    previewCells,
    isWinner 
}) => {
    const height = parseInt(boardHeight);
    const width = parseInt(boardWidth);
    const grid = Array.from({ length: height }, () => Array(width).fill(null));
    
    cells.forEach(cell => {
        if (!cell) return;

        const { row, col } = cell;

        if (row >= 0 && row < height && col >= 0 && col < width) {
            grid[row][col] = cell;
        }
    });

    return (
        <div className="flex-1 p-6 bg-white rounded-lg shadow-md">
            <div className="mb-4">
                <div className="flex items-center gap-2 mb-1">
                    <h2 className="text-2xl font-bold text-gray-800">{name}</h2>
                    <span className={`px-2 py-1 text-xs font-semibold rounded ${isPlayer ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'}`}>
                        {isPlayer ? 'YOU' : 'ENEMY'}
                    </span>
                    {isWinner === true && <TrophyIcon size={24} weight="fill" className="text-yellow-500" />}
                    {isWinner === false && <SmileySadIcon size={24} weight="fill" className="text-gray-400" />}
                </div>
                <p className="text-sm text-gray-600">Score: <span className="font-semibold text-blue-600">{score || 0}</span></p>
            </div>
            <div className={`grid ${(width > 10 && height > 10) ? 'gap-0.5' : 'gap-1'}`} style={{gridTemplateColumns: `repeat(${width}, minmax(0, 1fr))`, maxWidth: `${width * 46}px`}}>
                {grid.map((row, rowIdx) => 
                    row.map((cell, colIdx) => {
                        const isPreview = isPlayer && previewCells.some(p => p.row === rowIdx && p.col === colIdx);
                        return (
                            <BoardCell 
                                key={`${rowIdx}-${colIdx}`} 
                                row={rowIdx} 
                                col={colIdx} 
                                cell={cell}
                                height={height}
                                width={width}
                                isPlayer={isPlayer} 
                                gameState={gameState} 
                                handleCellClick={handleCellClick}
                                isPreview={isPreview}
                            />
                        );
                    })
                )}
            </div>
            
            {/* LEGEND */}
            <div className="flex flex-wrap gap-3 mt-4 text-xs">
                <div className="flex items-center gap-1.5">
                    <div className="w-4 h-4 bg-blue-200 border border-gray-400 rounded-[4px]"></div>
                    <span className="text-gray-600">Water</span>
                </div>
                {isPlayer && (
                    <div className="flex items-center gap-1.5">
                        <div className="w-4 h-4 bg-gray-400 border border-gray-400 rounded-[4px]"></div>
                        <span className="text-gray-600">Your Ship</span>
                    </div>
                )}
                <div className="flex items-center gap-1.5">
                    <div className="w-4 h-4 bg-gray-500 border border-gray-400 rounded-[4px]"></div>
                    <span className="text-gray-600">Miss</span>
                </div>
                <div className="flex items-center gap-1.5">
                    <div className="w-4 h-4 bg-red-500 border border-gray-400 rounded-[4px]"></div>
                    <span className="text-gray-600">Hit</span>
                </div>
                <div className="flex items-center gap-1.5">
                    <div className="w-4 h-4 bg-red-600 border border-gray-400 rounded-[4px]"></div>
                    <span className="text-gray-600">Sunk</span>
                </div>
            </div>
        </div>
    );
};
