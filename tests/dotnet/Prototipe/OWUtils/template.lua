{%
local method = _ctx.var.method or 'unknown'
local modified_body
if _body and _body ~= '' then
    modified_body = _body:gsub('^%{', '{\"method\": \"' .. method .. '\", ')
else
    modified_body = '{\"method\": \"' .. method .. '\"}'
end
%}
{* modified_body *}
