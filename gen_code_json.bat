set WORKSPACE=.

set GEN_CLIENT=%WORKSPACE%\Tools\Luban\Tools\Luban.ClientServer\Luban.ClientServer.exe
set CONF_ROOT=%WORKSPACE%\Design

%GEN_CLIENT% -j cfg --^
    -d %CONF_ROOT%\Defines\__root__.xml ^
    --input_data_dir %CONF_ROOT%\Datas ^
    --output_code_dir Unity\Assets\Scripts\Game\Generate ^
    --output_data_dir Unity\Assets\Res\Generate\json ^
    --gen_types code_cs_unity_json,data_json ^
    -s all 
pause