export const Modal = ({ isOpen, onClose, type, title, message }) => {
    if (!isOpen) return null;

    const isVictory = type.includes('game-over');
    const isStart = type.includes('battle-start');

    return (
        <div className={`fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50`}>
            <div className={`w-full max-w-md p-6 bg-white rounded-lg shadow-xl ${isVictory || isStart ? 'animate-bounce-in' : ''}`}>
                <h2 className={`mb-4 text-2xl font-bold ${isVictory ? 'text-green-600' : 'text-red-600'}`}>{title}</h2>
                <div className="mb-6 text-gray-700 whitespace-pre-line">{message}</div>
                <button 
                    onClick={onClose}
                    className="w-full px-4 py-2 text-white font-semibold bg-blue-500 rounded-lg hover:bg-blue-600 active:scale-[0.98] transition-all shadow-md hover:shadow-lg"
                >
                    OK
                </button>
            </div>
        </div>
    );
};
