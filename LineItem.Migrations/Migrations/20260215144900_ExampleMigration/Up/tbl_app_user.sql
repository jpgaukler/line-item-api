SET search_path TO lineitem;

CREATE TABLE app_user (
    id              BIGINT NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    external_id     UUID NOT NULL UNIQUE, -- ID from identity provider
    display_name    VARCHAR(100) NOT NULL,
    created_date    TIMESTAMPTZ NOT NULL DEFAULT now()
);

