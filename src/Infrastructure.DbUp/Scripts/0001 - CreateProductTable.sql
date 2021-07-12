﻿CREATE TABLE public.products
(
    id integer NOT NULL GENERATED BY DEFAULT AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
    name text,
    barcode text,
    description text,
    rate numeric(18,6) NOT NULL,
    created_by text,
    created timestamp without time zone NOT NULL,
    last_modified_by text,
    last_modified timestamp without time zone,
    CONSTRAINT pk_products PRIMARY KEY (id)
)