-- 50フレAボタン押しっぱなし
for i = 1, 10 do
    joypad.set({A=true})
    emu.frameadvance()
end

-- Aボタン離す
joypad.set({A=false})
emu.frameadvance()