import { SpeakerHighIcon, SpeakerSlashIcon, MusicNotesIcon, SpeakerXIcon } from '@phosphor-icons/react';

export const PauseMenu = ({ 
    isOpen, 
    onClose, 
    musicVolume, 
    setMusicVolume, 
    sfxVolume, 
    setSfxVolume,
    isMusicMuted,
    setIsMusicMuted,
    isSfxMuted,
    setIsSfxMuted,
    onResetGame 
}) => {
    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
            <div className="w-full max-w-md p-6 bg-white rounded-lg shadow-xl">
                <h2 className="mb-6 text-2xl font-bold text-center text-gray-800">Game Paused</h2>
                
                <div className="space-y-6">
                    <div>
                        <div className="flex items-center justify-between mb-3">
                            <div className="flex items-center gap-2">
                                <MusicNotesIcon size={20} className="text-blue-600" />
                                <span className="font-medium text-gray-700">Background Music</span>
                            </div>
                            <button
                                onClick={() => setIsMusicMuted(!isMusicMuted)}
                                className={`p-2 rounded-lg transition-colors ${
                                    isMusicMuted ? 'bg-red-100 text-red-600' : 'bg-blue-100 text-blue-600'
                                }`}
                            >
                                {isMusicMuted ? <SpeakerXIcon size={20} /> : <SpeakerHighIcon size={20} />}
                            </button>
                        </div>
                        <input
                            type="range"
                            min="0"
                            max="1"
                            step="0.1"
                            value={musicVolume}
                            onChange={(e) => setMusicVolume(parseFloat(e.target.value))}
                            disabled={isMusicMuted}
                            className="w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer slider"
                        />
                        <div className="flex justify-between text-xs text-gray-500 mt-1">
                            <span>0%</span>
                            <span>{Math.round(musicVolume * 100)}%</span>
                            <span>100%</span>
                        </div>
                    </div>

                    <div>
                        <div className="flex items-center justify-between mb-3">
                            <div className="flex items-center gap-2">
                                <SpeakerHighIcon size={20} className="text-green-600" />
                                <span className="font-medium text-gray-700">Sound Effects</span>
                            </div>
                            <button
                                onClick={() => setIsSfxMuted(!isSfxMuted)}
                                className={`p-2 rounded-lg transition-colors ${
                                    isSfxMuted ? 'bg-red-100 text-red-600' : 'bg-green-100 text-green-600'
                                }`}
                            >
                                {isSfxMuted ? <SpeakerSlashIcon size={20} /> : <SpeakerHighIcon size={20} />}
                            </button>
                        </div>
                        <input
                            type="range"
                            min="0"
                            max="1"
                            step="0.1"
                            value={sfxVolume}
                            onChange={(e) => setSfxVolume(parseFloat(e.target.value))}
                            disabled={isSfxMuted}
                            className="w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer slider"
                        />
                        <div className="flex justify-between text-xs text-gray-500 mt-1">
                            <span>0%</span>
                            <span>{Math.round(sfxVolume * 100)}%</span>
                            <span>100%</span>
                        </div>
                    </div>
                </div>

                <div className="flex gap-3 mt-8">
                    <button
                        onClick={onClose}
                        className="flex-1 px-4 py-2 text-white font-semibold bg-blue-500 rounded-lg hover:bg-blue-600 transition-colors"
                    >
                        Resume
                    </button>
                    <button
                        onClick={onResetGame}
                        className="flex-1 px-4 py-2 text-white font-semibold bg-red-500 rounded-lg hover:bg-red-600 transition-colors"
                    >
                        Reset Game
                    </button>
                </div>
            </div>
        </div>
    );
};