-- 50フレlidボタン押しっぱなし
for i = 1, 10 do
    joypad.set({lid=true})
    emu.frameadvance()
end

-- lidボタン離す
joypad.set({lid=false})
emu.frameadvance()