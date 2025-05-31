-- 50フレBボタン押しっぱなし
for i = 1, 10 do
    joypad.set({B=true})
    emu.frameadvance()
end

-- Bボタン離す
joypad.set({B=false})
emu.frameadvance()