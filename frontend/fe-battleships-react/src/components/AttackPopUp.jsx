import { CrosshairSimpleIcon, TargetIcon } from "@phosphor-icons/react";

export const AttackPopUp = ({ title, notif }) => {
    if (!notif) return null;

    const isHit = notif.isHit;

    return (
        <div className="absolute top-4 right-4 z-10 animate-bounce-in pointer-events-none">
            <div
                className={`
                    flex items-center gap-3 
                    px-4 py-2 
                    rounded-full 
                    shadow-md
                    w-64
                    justify-start 
                    bg-white/95 
                    border 
                    ${isHit ? 'border-green-500 text-green-700' : 'border-slate-300 text-slate-600'}
                `}
            >
                {isHit ? (
                    <TargetIcon size={28} weight="fill" />
                ) : (
                    <CrosshairSimpleIcon size={28} />
                )}

                <div className="flex flex-col leading-tight">
                    {/* <span className="text-xs font-semibold tracking-wide uppercase">
                        {title}
                    </span> */}
                    <span className="text-xs md:text-lg font-extrabold tracking-wide uppercase">
                        {isHit ? 'Hit confirmed!' : 'Shot missed!'}
                    </span>
                    <span className="text-[11px] text-slate-500 mt-1.5 pb-[4px] line-clamp-1">
                        {notif.message}
                    </span>
                </div>
            </div>
        </div>
    );
};
