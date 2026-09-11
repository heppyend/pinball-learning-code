#pragma once

#include "stdint.h"
#include <stdlib.h>

/* value at a non-valid index */
#define NONVALIDVALUE		NULL//cast(TValue *, luaO_nilobject)

/* test for pseudo index */
#define ispseudo(i)		((i) <= LUA_REGISTRYINDEX)

#if LOCAL_DEBUG_USE_LUA_VERSION == 503
#include "lua53/lua.h"
#include "lua53/lobject.h"
#include "lua53/lstate.h"
#include "lua53/lfunc.h"
#include "lua53/lapi.h"
#include "lua53/lstring.h"
#include "lua53/ltable.h"
#include "lua53/lauxlib.h"
#elif LOCAL_DEBUG_USE_LUA_VERSION == 501
#include "lua51/lua.h"
#include "lua51/lobject.h"
#include "lua51/lstate.h"
#include "lua51/lfunc.h"
#include "lua51/lapi.h"
#include "lua51/lstring.h"
#include "lua51/ltable.h"
#include "lua51/lauxlib.h"
#elif __EMSCRIPTEN__
//EMSCRIPTEN_ENV_LUA_IMPORT_LOGIC_START
#include "D:\\unity project\\pinball\\build\\luajit-2.1.0b3\\src\\lua.h"
#include "D:\\unity project\\pinball\\build\\lua-5.3.5\\src\\lobject.h"
#include "D:\\unity project\\pinball\\build\\lua-5.3.5\\src\\lstate.h"
#include "D:\\unity project\\pinball\\build\\lua-5.3.5\\src\\lfunc.h"
#include "D:\\unity project\\pinball\\build\\lua-5.3.5\\src\\lapi.h"
#include "D:\\unity project\\pinball\\build\\lua-5.3.5\\src\\lstring.h"
#include "D:\\unity project\\pinball\\build\\lua-5.3.5\\src\\ltable.h"
#include "D:\\unity project\\pinball\\build\\luajit-2.1.0b3\\src\\lauxlib.h"
//EMSCRIPTEN_ENV_LUA_IMPORT_LOGIC_END
#endif
