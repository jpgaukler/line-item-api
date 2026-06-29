CREATE OR REPLACE FUNCTION lineitem.app_user_retrieve_by_id(p_id BIGINT)
  RETURNS SETOF lineitem.app_user AS
$$
BEGIN
  RETURN QUERY
    SELECT * FROM lineitem.app_user WHERE id = p_id;
END;
$$ LANGUAGE plpgsql STABLE;
