-- 50フレRボタン押しっぱなし
for i = 1, 10 do
    joypad.set({R=true})
    emu.frameadvance()
end

-- Rボタン離す
joypad.set({R=false})
emu.frameadvance()