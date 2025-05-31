-- 50フレrightボタン押しっぱなし
for i = 1, 10 do
    joypad.set({right=true})
    emu.frameadvance()
end

-- rightボタン離す
joypad.set({right=false})
emu.frameadvance()