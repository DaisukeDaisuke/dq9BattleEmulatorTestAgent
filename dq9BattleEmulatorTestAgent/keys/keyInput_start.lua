-- 50フレstartボタン押しっぱなし
for i = 1, 10 do
    joypad.set({start=true})
    emu.frameadvance()
end

-- startボタン離す
joypad.set({start=false})
emu.frameadvance()