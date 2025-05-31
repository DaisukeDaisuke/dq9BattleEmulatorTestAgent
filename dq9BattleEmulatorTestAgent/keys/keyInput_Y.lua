-- 50フレYボタン押しっぱなし
for i = 1, 10 do
    joypad.set({Y=true})
    emu.frameadvance()
end

-- Yボタン離す
joypad.set({Y=false})
emu.frameadvance()