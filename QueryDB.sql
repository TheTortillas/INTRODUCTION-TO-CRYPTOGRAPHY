# ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ CREACIÓN E INICIALIZACIÓN DE LA BASE DE DATOS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
CREATE DATABASE intro_to_crypto;
USE intro_to_crypto;

# ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ TABLAS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
CREATE TABLE users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    second_last_name VARCHAR(50) NOT NULL,
    email  VARCHAR(50) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    salt VARCHAR(255) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

# ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ PROCEDMIENTOS ALMACENADOS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
DELIMITER $$
CREATE PROCEDURE sp_insert_user(
    IN p_first_name VARCHAR(50),
    IN p_last_name VARCHAR(50),
    IN p_second_last_name VARCHAR(50),
    IN p_email VARCHAR(50),
    IN p_password VARCHAR(255),
    IN p_salt VARCHAR(255),
    OUT p_status_code INT,
    OUT p_message VARCHAR(255)
)
BEGIN
    DECLARE email_count INT DEFAULT 0;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION 
    BEGIN
        ROLLBACK;
        SET p_status_code = -1;
        SET p_message = 'Error: SQL EXCEPTION.';
    END;

    START TRANSACTION;
    
    SELECT COUNT(*) INTO email_count FROM users WHERE email = p_email;

    IF email_count > 0 THEN
        SET p_status_code = 0;
        SET p_message = 'Error: El correo ya está registrado.';
    ELSE
        -- Ahora insertamos el password hasheado directamente sin SHA2
        INSERT INTO users (first_name, last_name, second_last_name, email, password, salt)
        VALUES (p_first_name, p_last_name, p_second_last_name, p_email, p_password, p_salt);

        SET p_status_code = 1;
        SET p_message = 'Éxito: Usuario insertado correctamente.';
    END IF;

    COMMIT;
END $$
DELIMITER ;

-- También hay que modificar el procedimiento de login
DELIMITER $$
CREATE PROCEDURE sp_login_user(
    IN p_email VARCHAR(50),
    IN p_password VARCHAR(255),
    OUT p_user_id INT,
    OUT p_status_code INT,
    OUT p_message VARCHAR(255)
)
BEGIN
    DECLARE v_stored_password VARCHAR(255);
    DECLARE v_stored_salt VARCHAR(255);
    DECLARE v_user_id INT;

    START TRANSACTION;
    
    SELECT id, password, salt INTO v_user_id, v_stored_password, v_stored_salt
    FROM users 
    WHERE email = p_email;
    
    IF v_stored_password IS NULL THEN
        SET p_status_code = -1;
        SET p_message = 'Error: Usuario no encontrado.';
        SET p_user_id = NULL;
    ELSE
        -- La verificación de la contraseña se hará en el backend
        SET p_status_code = 1;
        SET p_message = 'Éxito: Usuario encontrado.';
        SET p_user_id = v_user_id;
    END IF;

    COMMIT;
END $$
DELIMITER ;

DELIMITER $$
CREATE PROCEDURE sp_get_user_by_id(
    IN p_user_id INT,
    OUT p_status_code INT,
    OUT p_message VARCHAR(255)
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION 
    BEGIN
        ROLLBACK;
        SET p_status_code = -1;
        SET p_message = 'Error: Ocurrió un error en la base de datos.';
        SELECT NULL as id, NULL as first_name, NULL as last_name, 
               NULL as second_last_name, NULL as email, NULL as created_at;
    END;

    START TRANSACTION;
    
    -- Verificar si existe el usuario
    IF NOT EXISTS (SELECT 1 FROM users WHERE id = p_user_id) THEN
        SET p_status_code = -1;
        SET p_message = 'Error: Usuario no encontrado.';
        SELECT NULL as id, NULL as first_name, NULL as last_name, 
               NULL as second_last_name, NULL as email, NULL as created_at;
    ELSE
        SET p_status_code = 1;
        SET p_message = 'Éxito: Usuario encontrado.';
        
        SELECT id, first_name, last_name, second_last_name, 
               email, created_at
        FROM users 
        WHERE id = p_user_id;
    END IF;

    COMMIT;
END $$
DELIMITER ;

DELIMITER $$
CREATE PROCEDURE sp_get_user_by_email(
    IN p_email VARCHAR(50)
)
BEGIN
    SELECT id, first_name, last_name, second_last_name, email, password, salt
    FROM users 
    WHERE email = p_email;
END $$
DELIMITER ;

# ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ PRUEBAS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
select * from users;
-- drop database intro_to_crypto;
-- truncate table users;