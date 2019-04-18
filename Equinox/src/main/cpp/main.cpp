// Entry-point for the Equinox application
// Author	: Malachi Austin
// Version  : v1.0.0 (SNAPSHOT)
// Revision : 1

#include "Utilities.hpp"
#include <string>

using namespace Luna;
using namespace std;

static Utilities* Util;

int main(string argc, int argv) {
	Util = new Utilities();

	delete Util;
	return EXIT;
}