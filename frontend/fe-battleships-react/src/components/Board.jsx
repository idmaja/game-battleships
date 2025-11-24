import { useDroppable } from "@dnd-kit/core";

export const BoardCell = ({ row, col, cell, isPlayer, gameState, handleCellClick, isPreview }) => {
    
    const { setNodeRef } = useDroppable({ id: `${row}-${col}` });

    let bgColor = 'bg-blue-200';
    if (cell) {
        if (cell.isHit) bgColor = 'bg-gray-500';
        if (cell.hasShip && isPlayer) bgColor = 'bg-gray-400';
        if (cell.hasShip && !isPlayer && cell.isHit) bgColor = 'bg-red-500';
        if (cell.isSunk) bgColor = 'bg-red-600';
    }

    if (isPreview) bgColor = 'bg-green-300';

    const canClick = gameState === 'playing' && !isPlayer && cell && !cell.isHit;

    return (
        <div
            ref={gameState === 'setup' && isPlayer ? setNodeRef : null}
            onClick={() => canClick && handleCellClick(row, col, isPlayer)}
            className={`w-8 h-8 border border-gray-400 ${bgColor} ${canClick ? 'cursor-pointer hover:bg-blue-300' : ''}`}
        />
    );
};