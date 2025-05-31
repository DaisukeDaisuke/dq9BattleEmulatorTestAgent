-- 50フレleftボタン押しっぱなし
for i = 1, 10 do
    joypad.set({left=true})
    emu.frameadvance()
end

-- leftボタン離す
joypad.set({left=false})
emu.frameadvance()