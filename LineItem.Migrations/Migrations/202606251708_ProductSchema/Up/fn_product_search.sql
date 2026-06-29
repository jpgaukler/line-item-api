CREATE OR REPLACE FUNCTION lineitem.product_search(
  p_search_term VARCHAR
)
  RETURNS SETOF lineitem.product_version
AS
$$
BEGIN
  RETURN QUERY
    SELECT pv.*
    FROM lineitem.product_version pv
           INNER JOIN lineitem.product p
                      ON p.id = pv.product_id
                        AND p.active_version = pv.version
    WHERE WORD_SIMILARITY(p_search_term, p.name) > 0.3
       OR WORD_SIMILARITY(p_search_term, p.description) > 0.3
    ORDER BY GREATEST(
                 WORD_SIMILARITY(p_search_term, p.name),
                 WORD_SIMILARITY(p_search_term, p.description)
             ) DESC;
END;
$$ LANGUAGE plpgsql STABLE;
