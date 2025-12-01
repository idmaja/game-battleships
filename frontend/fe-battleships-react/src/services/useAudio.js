import { useState, useEffect } from 'react';
import { Howl } from 'howler';

export const useAudio = () => {
    const [musicVolume, setMusicVolume] = useState(0.3);
    const [sfxVolume, setSfxVolume] = useState(0.5);
    const [isMusicMuted, setIsMusicMuted] = useState(false);
    const [isSfxMuted, setIsSfxMuted] = useState(false);
    
    const [backgroundMusic] = useState(() => new Howl({
        src: ['/sounds/background.mp3'],
        loop: true,
        volume: musicVolume
    }));

    const [sounds] = useState(() => ({
        attack: new Howl({ src: ['/sounds/attack.mp3'], volume: sfxVolume }),
        hit: new Howl({ src: ['/sounds/hit.mp3'], volume: sfxVolume }),
        miss: new Howl({ src: ['/sounds/miss.mp3'], volume: sfxVolume }),
        win: new Howl({ src: ['/sounds/win.mp3'], volume: sfxVolume }),
        lose: new Howl({ src: ['/sounds/lose.mp3'], volume: sfxVolume }),
        battle: new Howl({ src: ['/sounds/battle.mp3'], volume: sfxVolume }),
        'drag-start-ship': new Howl({ src: ['/sounds/drag-start-ship.mp3'], volume: sfxVolume }),
        'drag-end-ship': new Howl({ src: ['/sounds/drag-end-ship.mp3'], volume: sfxVolume })
    }));

    useEffect(() => {
        backgroundMusic.volume(isMusicMuted ? 0 : musicVolume);
    }, [musicVolume, isMusicMuted, backgroundMusic]);

    useEffect(() => {
        Object.values(sounds).forEach(sound => {
            sound.volume(isSfxMuted ? 0 : sfxVolume);
        });
    }, [sfxVolume, isSfxMuted, sounds]);

    const playBackgroundMusic = () => {
        if (!isMusicMuted) backgroundMusic.play();
    };

    const stopBackgroundMusic = () => {
        backgroundMusic.stop();
    };

    const playSfx = (soundName) => {
        if (sounds[soundName] && !isSfxMuted) {
            sounds[soundName].play();
        }
    };

    return {
        musicVolume,
        setMusicVolume,
        sfxVolume,
        setSfxVolume,
        isMusicMuted,
        setIsMusicMuted,
        isSfxMuted,
        setIsSfxMuted,
        playBackgroundMusic,
        stopBackgroundMusic,
        playSfx
    };
};