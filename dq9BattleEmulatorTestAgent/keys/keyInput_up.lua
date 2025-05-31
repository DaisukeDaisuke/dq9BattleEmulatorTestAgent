-- 50フレupボタン押しっぱなし
for i = 1, 10 do
    joypad.set({up=true})
    emu.frameadvance()
end

-- upボタン離す
joypad.set({up=false})
emu.frameadvance()