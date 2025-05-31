-- 50フレdownボタン押しっぱなし
for i = 1, 10 do
    joypad.set({down=true})
    emu.frameadvance()
end

-- downボタン離す
joypad.set({down=false})
emu.frameadvance()