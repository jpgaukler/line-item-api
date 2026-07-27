CREATE OR REPLACE FUNCTION lineitem.product_retrieve_specific_version_by_id(
  p_product_id BIGINT,
  p_version INT
)
  RETURNS SETOF lineitem.product_version
AS
$$
BEGIN
  RETURN QUERY
    SELECT *
    FROM lineitem.product_version
    WHERE product_id = p_product_id
      AND version = p_version;
END;
$$ LANGUAGE plpgsql STABLE;


CREATE OR REPLACE FUNCTION lineitem.product_retrieve_specific_version_by_id(
  p_product_id BIGINT,
  p_version INT
)
  RETURNS TABLE
          (
            id                  BIGINT,
            product_category_id BIGINT,
            version             INT,
            product_data        JSONB,
            created_at          TIMESTAMPTZ,
            created_by          BIGINT
          )
AS
$$
BEGIN
  RETURN QUERY
    SELECT p.id,
           p.product_category_id,
--            p.name,
--            p.description,
           pv.version,
           pv.product_data,
           pv.created_at,
           pv.created_by
    FROM lineitem.product p
           INNER JOIN lineitem.product_version pv
                      ON pv.product_id = p.id
                        AND pv.version = p_version
    WHERE p.id = p_product_id;
END;
$$ LANGUAGE plpgsql STABLE;