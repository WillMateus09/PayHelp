-- Script para inserir o usuário Master Admin
-- Execute este script no banco de dados PayHelp_Banco

-- Verifica se o usuário já existe antes de inserir
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'payhelp.master@gmail.com')
BEGIN
    INSERT INTO Users (Id, NumeroInscricao, Nome, Email, SenhaHash, Role, IsBlocked)
    VALUES (
        NEWID(),
        '9999',
        'Master Admin',
        'payhelp.master@gmail.com',
        '4F1BEE2FC53F873E3B5BE41B01C049E7D789F6E29EDDB383738127618AE7CAC2',
        2, -- Master role
        0  -- Not blocked
    );
    
    PRINT 'Usuário Master Admin criado com sucesso!';
END
ELSE
BEGIN
    -- Se já existe, atualiza a senha para garantir que está correta
    UPDATE Users
    SET SenhaHash = '4F1BEE2FC53F873E3B5BE41B01C049E7D789F6E29EDDB383738127618AE7CAC2',
        Role = 2,
        IsBlocked = 0
    WHERE Email = 'payhelp.master@gmail.com';
    
    PRINT 'Usuário Master Admin atualizado com sucesso!';
END

-- Verifica o resultado
SELECT Id, NumeroInscricao, Nome, Email, Role, IsBlocked
FROM Users
WHERE Email = 'payhelp.master@gmail.com';
