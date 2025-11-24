import { useDraggable } from '@dnd-kit/core';
import ship3 from '../assets/ship-3.svg';
import ship4 from '../assets/ship-4.svg';
import ship5 from '../assets/ship-5.svg';

const shipSprites = {
    3: ship3,
    4: ship4,
    5: ship5,
};

export const DraggableShip = ({ length, index, shipOrientation }) => {
    const { attributes, listeners, setNodeRef, transform } =
        useDraggable({ id: `ship-${length}-${index}` });

    const style = {
        transform: transform 
        ? `translate3d(${transform.x}px, ${transform.y}px, 0)` 
        : undefined,
    };

    const sprite = shipSprites[length];

    return (
        <div
            ref={setNodeRef}
            style={style}
            {...listeners}
            {...attributes}
            className="inline-block cursor-move select-none"
        >
            <img
                src={sprite}
                alt={`ship-${length}`}
                className="pointer-events-none"
                style={{
                    width: shipOrientation === 'vertical' ? '100px' : `${length * 32}px`,
                    height: shipOrientation === 'vertical' ? `${length * 32}px` : '100px',
                    transform: shipOrientation === 'vertical' ? 'rotate(90deg)' : 'rotate(0deg)',
                    transition: 'transform 0.3s'
                }}
            />
        </div>
    );
};
