export const Modal = ({ isOpen, onClose, title, message }) => {
    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
            <div className="w-full max-w-md p-6 bg-white rounded-lg shadow-xl">
                <h2 className="mb-4 text-xl font-bold text-red-600">{title}</h2>
                <p className="mb-6 text-gray-700">{message}</p>
                <button 
                    onClick={onClose}
                    className="w-full px-4 py-2 text-white bg-blue-500 rounded hover:bg-blue-600"
                >
                    OK
                </button>
            </div>
        </div>
    );
};
