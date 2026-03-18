-- Official Oracle image runs startup scripts in the CDB context.
-- We must switch to FREEPDB1 before creating the app user.
-- The -lite image has no USERS tablespace, so we use SYSAUX as the default.

ALTER SESSION SET CONTAINER = FREEPDB1;

DECLARE
    v_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_count FROM all_users WHERE username = 'MEMBER_APP';
    IF v_count = 0 THEN
        EXECUTE IMMEDIATE 'CREATE USER MEMBER_APP IDENTIFIED BY MemberAppPass1 DEFAULT TABLESPACE SYSAUX TEMPORARY TABLESPACE TEMP';
        EXECUTE IMMEDIATE 'GRANT CONNECT, RESOURCE TO MEMBER_APP';
        EXECUTE IMMEDIATE 'ALTER USER MEMBER_APP QUOTA UNLIMITED ON SYSAUX';
    END IF;
END;
/
