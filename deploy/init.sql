CREATE UNLOGGED TABLE transacoes (
	id SERIAL PRIMARY KEY,
	cliente_id INTEGER NOT NULL,
	valor INTEGER NOT NULL,
	tipo CHAR(1) NOT NULL,
	descricao VARCHAR(10) NOT NULL,
	realizada_em TIMESTAMP NOT NULL DEFAULT NOW()
);
CREATE INDEX idx_transacoes_ids_cliente_id ON transacoes (cliente_id);

CREATE UNLOGGED TABLE saldos (
	id SERIAL PRIMARY KEY,
	cliente_id INTEGER NOT NULL,
	valor INTEGER NOT NULL,
	limite INTEGER NOT NULL
    constraint saldos_limite_check check (valor >= -limite)
);
CREATE INDEX idx_saldos_ids_cliente_id ON saldos (cliente_id);

CREATE OR REPLACE FUNCTION creditar(c_id INT, credito INT, descricao VARCHAR(10)) 
	RETURNS TABLE (saldo int, erro BOOL)  LANGUAGE plpgsql AS
$$
BEGIN
	PERFORM pg_advisory_xact_lock(c_id);
	
    INSERT INTO transacoes (cliente_id, valor, tipo, descricao, realizada_em) VALUES (c_id, credito, 'c', descricao, now());
    RETURN QUERY
		UPDATE saldos SET valor = valor + credito WHERE cliente_id = c_id 
		RETURNING valor, FALSE;
END;
$$;

CREATE OR REPLACE FUNCTION debitar(c_id INT, debito INT, , descricao VARCHAR(10)) 
    RETURNS TABLE (saldo int, erro BOOL) LANGUAGE plpgsql AS 
$$
DECLARE 
	saldo int;
BEGIN
	PERFORM pg_advisory_xact_lock(c_id);
	BEGIN
		INSERT INTO transacoes
			VALUES(DEFAULT, c_id, debito, 'd', descricao, NOW());
		
		UPDATE saldos
		SET valor = valor - debito
		WHERE cliente_id = c_id;

		RETURN QUERY
			SELECT
				valor,
				FALSE
			FROM saldos
			WHERE cliente_id = c_id;
	EXCEPTION WHEN OTHERS THEN 
	RETURN QUERY
			SELECT
				valor,
				FALSE
			FROM saldos
			WHERE cliente_id = c_id;
	END;
END;
$$;

DO $$ BEGIN 
	INSERT INTO saldos (cliente_id,valor, limite) 
	VALUES (1, 0, 1000 * 100 ),
		   (2, 0, 800 * 100),
		   (3, 0, 10000  * 100),
		   (4, 0, 100000 * 100),
		   (5, 0, 5000 * 100 );
END;
$$;
