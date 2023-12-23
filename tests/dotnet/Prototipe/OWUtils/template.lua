{%
local method = _ctx.var.method or 'unknown'
local modified_body

-- Funzione per analizzare la stringa di query in una tabella
local function parse_query(query)
    local params = {}
    for k, v in string.gmatch(query, "([^&=]+)=([^&=]+)") do
        params[k] = v
    end
    return params
end

-- Recupera e analizza i parametri dell'URL
local url_params_str = _ctx.var.args or ""
local url_params = parse_query(url_params_str)

-- Converti i parametri dell'URL in una stringa JSON
local url_params_json = ""
for k, v in pairs(url_params) do
    url_params_json = url_params_json .. '\"' .. k .. '\": \"' .. v .. '\", '
end
url_params_json = url_params_json:sub(1, -3)  -- Rimuove l'ultima virgola

-- Costruisci il corpo della richiesta modificato
if _body and _body ~= '' then
    modified_body = _body:gsub('^%{', '{\"method\": \"' .. method .. '\", ' .. url_params_json .. ', ')
else
    modified_body = '{\"method\": \"' .. method .. '\", ' .. url_params_json .. '}'
end
%}
{* modified_body *}
