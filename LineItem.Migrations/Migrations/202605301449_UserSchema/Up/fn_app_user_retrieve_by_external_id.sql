CREATE OR REPLACE FUNCTION lineitem.app_user_retrieve_by_external_id(p_external_id VARCHAR)
  RETURNS SETOF lineitem.app_user AS
$$
BEGIN
  RETURN QUERY
    SELECT * FROM lineitem.app_user WHERE external_id = p_external_id;
END;
$$ LANGUAGE plpgsql;
