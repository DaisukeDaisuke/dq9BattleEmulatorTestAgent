-- 50フレdebugボタン押しっぱなし
for i = 1, 10 do
    joypad.set({debug=true})
    emu.frameadvance()
end

-- debugボタン離す
joypad.set({debug=false})
emu.frameadvance()