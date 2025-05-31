-- 50フレselectボタン押しっぱなし
for i = 1, 10 do
    joypad.set({select=true})
    emu.frameadvance()
end

-- selectボタン離す
joypad.set({select=false})
emu.frameadvance()