-- 50フレLボタン押しっぱなし
for i = 1, 10 do
    joypad.set({L=true})
    emu.frameadvance()
end

-- Lボタン離す
joypad.set({L=false})
emu.frameadvance()