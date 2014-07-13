///////////////////////////////////////////////////////////////////////
// @brief Low level mouse hook to allow scrolling with a trackball without a scroll wheel.
// Converts XBUTTONDOWN + MOUSEMOVE into MOUSEEVENTF_WHEEL.
// Converts XBUTTONDOWN + XBUTTONUP into MOUSEEVENTF_MIDDLEDOWN + MOUSEEVENTF_MIDDLEUP.
// @author Martin Seelge
// @date 2014-07-12
///////////////////////////////////////////////////////////////////////
#include <windows.h>
const int Y_THRESHOLD = 10;   // threshold in pixels to trigger wheel event
const DWORD WHEEL_FACTOR = 1; // number of wheel events. The lines scrolled per wheel event are determined by the Microsoft Windows mouse wheel settings.
enum State {
	NORMAL = 0, // default state
	DOWN,       // mouse XButton pressed, no movement
	SCROLL,     // mouse XButton pressed + moved
};
HHOOK g_Hook;           // callback function hook, called for every mouse event
State g_state = NORMAL; // initial state
POINT g_origin;         // cursor position when entering state DOWN
int g_ycount = 0;       // accumulated vertical movement while in state SCROLL

LRESULT CALLBACK LowLevelMouseProc(int nCode, WPARAM wParam, LPARAM lParam) {
	if (nCode != HC_ACTION)
		return CallNextHookEx(g_Hook, nCode, wParam, lParam);
	BOOL preventCallNextHookEx = FALSE;
	const MSLLHOOKSTRUCT *p = reinterpret_cast<const MSLLHOOKSTRUCT*>(lParam);
	switch (g_state) {
	case NORMAL:
		if (wParam == WM_XBUTTONDOWN) { // NORMAL->DOWN: remember position
			preventCallNextHookEx = TRUE;
			g_state = DOWN;
			g_origin = p->pt;
		}
		break;
	case DOWN:
		if (wParam == WM_XBUTTONUP) { // DOWN->NORMAL: middle button click
			preventCallNextHookEx = TRUE;
			g_state = NORMAL;
			INPUT input[2];
			input[0].type = INPUT_MOUSE;
			input[0].mi.dx = p->pt.x;
			input[0].mi.dy = p->pt.y;
			input[0].mi.mouseData = (DWORD)0x0;
			input[0].mi.dwFlags = MOUSEEVENTF_MIDDLEDOWN; // middle button down
			input[0].mi.time = (DWORD)0x0;
			input[0].mi.dwExtraInfo = (ULONG_PTR)NULL;
			input[1].type = INPUT_MOUSE;
			input[1].mi.dx = p->pt.x;
			input[1].mi.dy = p->pt.y;
			input[1].mi.mouseData = (DWORD)0x0;
			input[1].mi.dwFlags = MOUSEEVENTF_MIDDLEUP; // middle button up
			input[1].mi.time = (DWORD)0x0;
			input[1].mi.dwExtraInfo = (ULONG_PTR)NULL;
			SendInput(2, input, sizeof(INPUT));
		}
		else if (wParam == WM_MOUSEMOVE) { // DOWN->SCROLL
			preventCallNextHookEx = TRUE;
			g_state = SCROLL;
			g_ycount = 0;
			SetCursorPos(g_origin.x, g_origin.y);
		}
		break;
	case SCROLL:
		if (wParam == WM_XBUTTONUP) { // SCROLL->NORMAL
			preventCallNextHookEx = TRUE;
			g_state = NORMAL;
		}
		else if (wParam == WM_MOUSEMOVE) { // SCROLL->SCROLL: wheel event
			preventCallNextHookEx = TRUE;
			g_ycount += p->pt.y - g_origin.y;
			SetCursorPos(g_origin.x, g_origin.y);
			if (g_ycount < -Y_THRESHOLD || g_ycount > Y_THRESHOLD){
				DWORD mouseData = (g_ycount > 0 ? -WHEEL_DELTA : +WHEEL_DELTA); // scroll direction
				g_ycount = 0;
				INPUT input[WHEEL_FACTOR];
				for (size_t i = 0; i < WHEEL_FACTOR; ++i) {
					input[i].type = INPUT_MOUSE;
					input[i].mi.dx = p->pt.x;
					input[i].mi.dy = p->pt.y;
					input[i].mi.mouseData = mouseData;
					input[i].mi.dwFlags = MOUSEEVENTF_WHEEL; // wheel
					input[i].mi.time = (DWORD)0x0;
					input[i].mi.dwExtraInfo = (ULONG_PTR)NULL;
				}
				SendInput(WHEEL_FACTOR, input, sizeof(INPUT));
			}
		}
		break;
	}
	return (preventCallNextHookEx ? 1 : CallNextHookEx(g_Hook, nCode, wParam, lParam));
}

int main() {
	g_Hook = SetWindowsHookEx(WH_MOUSE_LL, &LowLevelMouseProc, GetModuleHandle(0), 0);
	if (!g_Hook) return 1; // hook failed
	MSG message;
	while (GetMessage(&message, NULL, 0, 0) > 0) // message pump, currently no exit event exists
		DispatchMessage(&message);
	UnhookWindowsHookEx(g_Hook); // remove hook
	return 0;
}
