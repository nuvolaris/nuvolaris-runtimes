{%
local function transform_body(_ctx, xbody)
    local method = _ctx.var.method or 'unknown'
    local modified_body = nil

    -- Funzione per analizzare la stringa di query in una tabella
    local function parse_query(query)
        local params = {}
        for k, v in string.gmatch(query, "([^&=]+)=([^&=]+)") do
            params[k] = v
        end
        return params
    end

    -- Recupera e analizza i parametri dell'URL
    local url_params_str = _ctx.var.args or ''
    local url_params = parse_query(url_params_str)

    -- Recupera e analizza gli header HTTP
    local headers = ngx.req.get_headers() or {}
	
    local headers_json = ''
    for k, v in pairs(headers) do
        headers_json = headers_json .. '\"' .. k .. '\": \"' .. v .. '\", '
    end
    headers_json = headers_json:sub(1, -3) -- Rimuove l'ultima virgola

    -- Converti i parametri dell'URL in una stringa JSON
    local url_params_json = ''
    for k, v in pairs(url_params) do
        url_params_json = url_params_json .. '\"' .. k .. '\": \"' .. v .. '\", '
    end
    url_params_json = url_params_json:sub(1, -3) -- Rimuove l'ultima virgola

    -- Costruisci il corpo della richiesta modificato
    if xbody and xbody ~= '' then
        xbody = xbody:gsub('\n', '') -- Rimuove i caratteri di nuova linea
        xbody = xbody:gsub('\r', '') -- Rimuove i caratteri di ritorno a capo
        local combined_params = ''
		if url_params_json and url_params_json ~= '' then
			if headers_json and headers_json ~= '' then
				combined_params = url_params_json .. ', ' .. headers_json
			else
				combined_params = url_params_json
			end
		else
			if headers_json and headers_json ~= '' then
				combined_params = headers_json
			else
				combined_params = ''
			end
		end
        if combined_params ~= '' then
            modified_body = xbody:gsub('^%{', '{\"method\": \"' .. method .. '\", ' .. combined_params .. ', ')
        else
            modified_body = xbody:gsub('^%{', '{\"method\": \"' .. method .. '\",')
        end
        _body = nil
        xbody = nil
        query = nil
        --error("1: " .. modified_body)
    else
        modified_body = '{\"method\": \"' .. method .. '\", ' .. url_params_json .. ', ' .. headers_json .. '}'
        _body = nil
        xbody = nil
        query = nil
        --error("2: " .. modified_body)
    end

    return modified_body
end
%}
{* transform_body(_ctx, _body) *}
