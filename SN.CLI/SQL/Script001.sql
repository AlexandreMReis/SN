CREATE SEQUENCE seq_members START 1;
CREATE SEQUENCE seq_books START 1;
CREATE SEQUENCE seq_are_friends START 1;
CREATE SEQUENCE seq_authors START 1;
CREATE SEQUENCE seq_rel_books_authors START 1;
CREATE SEQUENCE seq_readings START 1;

CREATE TABLE members(
	id int PRIMARY KEY,
	name VARCHAR (50) NOT NULL
);

CREATE TABLE are_friends(
	id int PRIMARY KEY,
	addressed_id int,
	requester_id int,
	FOREIGN KEY (addressed_id) REFERENCES members (id),
	FOREIGN KEY (requester_id) REFERENCES members (id),
	CHECK (addressed_id != requester_id)
);

CREATE TABLE books(
	id int PRIMARY KEY,
	title VARCHAR (50) NOT NULL
);

CREATE TABLE authors(
	id int PRIMARY KEY,
	name VARCHAR (50) NOT NULL
);

CREATE TABLE rel_books_authors(
	id int PRIMARY KEY,
	author_id int  NOT NULL,
	book_id int  NOT NULL,
	FOREIGN KEY (author_id) REFERENCES authors (id),
	FOREIGN KEY (book_id) REFERENCES books (id)
);

CREATE TABLE readings(
	id int PRIMARY KEY,
	reader_id int,
	book_id int,
	liked_rating int null,
	UNIQUE(reader_id, book_id),
	FOREIGN KEY (reader_id) REFERENCES members (id),
	FOREIGN KEY (book_id) REFERENCES books (id)
);
	
CREATE VIEW VW_BOOKS AS
	SELECT bks.ID, bks.TITLE, string_agg(aut.NAME, ', ') AS authors
	  FROM books bks
 LEFT JOIN rel_books_authors rba ON bks.id = rba.book_id
 LEFT JOIN authors aut ON rba.author_id = aut.id
  GROUP BY bks.ID;

CREATE OR REPLACE PROCEDURE SP_INSERT_BOOK(book_title varchar(50), authors_names varchar(50)[])
LANGUAGE plpgsql    
AS $$
DECLARE
	book_id integer;
	author_id integer;
	author_name varchar(50);
BEGIN

	IF EXISTS (SELECT id FROM books WHERE title = book_title) THEN
		RAISE EXCEPTION SQLSTATE '90001' USING MESSAGE = 'Book already exists';
	END IF;
	
	book_id := nextval('seq_books');
	INSERT INTO books (id, title) values (book_id, book_title);

	FOREACH author_name IN ARRAY authors_names
    LOOP
		SELECT INTO author_id id FROM authors WHERE name = author_name limit 1;
		
		IF author_id IS NULL THEN
			author_id := nextval('seq_authors');
			INSERT INTO authors(id, name) values(author_id, author_name);
		END IF;

		INSERT INTO rel_books_authors(id, author_id, book_id)
		SELECT nextval('seq_rel_books_authors'), author_id, book_id;
		
    END LOOP;
    COMMIT;
END;
$$;

CREATE OR REPLACE PROCEDURE SP_INSERT_READING(book_id integer, reader_id integer, liked_rating integer)
LANGUAGE plpgsql    
AS $$
BEGIN
	INSERT INTO readings(id, reader_id, book_id, liked_rating) VALUES (nextval('seq_readings'), reader_id, book_id, liked_rating);
		
    COMMIT;
END;
$$;

CREATE OR REPLACE PROCEDURE SP_INSERT_MEMBER(member_name varchar(50))
LANGUAGE plpgsql    
AS $$
BEGIN
	INSERT INTO members(id, name) VALUES (nextval('seq_members'), member_name);
    COMMIT;
END;
$$;

CREATE OR REPLACE PROCEDURE SP_INSERT_FRIENDSHIP(addressed_id integer, requester_id integer)
LANGUAGE plpgsql    
AS $$
BEGIN
	INSERT INTO are_friends(id, addressed_id, requester_id) VALUES (nextval('seq_are_friends'), addressed_id, requester_id);
    COMMIT;
END;
$$;


