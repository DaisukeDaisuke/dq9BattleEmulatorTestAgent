-- 50フレXボタン押しっぱなし
for i = 1, 10 do
    joypad.set({X=true})
    emu.frameadvance()
end

-- Xボタン離す
joypad.set({X=false})
emu.frameadvance()