CREATE OR REPLACE FUNCTION lineitem.product_category_retrieve_all()
  RETURNS SETOF lineitem.product_category
AS
$$
BEGIN
  RETURN QUERY
    SELECT *
    FROM lineitem.product_category
    ORDER BY name;
END;
$$ LANGUAGE plpgsql STABLE;