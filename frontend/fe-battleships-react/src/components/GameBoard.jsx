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
        <div className="flex-1 p-6 bg-white rounded shadow">
            <h2 className="mb-2 text-2xl font-bold">{name}</h2>
            <p className="mb-4 text-lg">Score: {score || 0}</p>
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
