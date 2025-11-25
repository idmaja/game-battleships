import { BoardCell } from './Board';

export const GameBoard = ({ 
    cells, 
    isPlayer, 
    name, 
    score, 
    boardWidth, 
    boardHeight, 
    gameState, 
    handleCellClick, 
    previewCells 
}) => {
    const height = parseInt(boardHeight);
    const width = parseInt(boardWidth);
    const grid = Array.from({ length: height }, () => Array(width).fill(null));
    
    cells.forEach(cell => {
        grid[cell.row][cell.col] = cell;
    });

    return (
        <div className="flex-1 p-6 bg-white rounded-lg shadow-md">
            <div className="mb-4">
                <div className="flex items-center gap-2 mb-1">
                    <h2 className="text-2xl font-bold text-gray-800">{name}</h2>
                    <span className={`px-2 py-1 text-xs font-semibold rounded ${isPlayer ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'}`}>
                        {isPlayer ? 'YOU' : 'ENEMY'}
                    </span>
                </div>
                <p className="text-sm text-gray-600">Score: <span className="font-semibold text-blue-600">{score || 0}</span></p>
            </div>
            <div className="grid gap-1" style={{gridTemplateColumns: `repeat(${width}, minmax(0, 1fr))`, maxWidth: `${width * 36}px`}}>
                {grid.map((row, rowIdx) => 
                    row.map((cell, colIdx) => {
                        const isPreview = isPlayer && previewCells.some(p => p.row === rowIdx && p.col === colIdx);
                        return (
                            <BoardCell 
                                key={`${rowIdx}-${colIdx}`} 
                                row={rowIdx} 
                                col={colIdx} 
                                cell={cell} 
                                isPlayer={isPlayer} 
                                gameState={gameState} 
                                handleCellClick={handleCellClick}
                                isPreview={isPreview}
                            />
                        );
                    })
                )}
            </div>
        </div>
    );
};
