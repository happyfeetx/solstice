#include "Utilities.hpp"
#include <ctime>
#include <cstdlib>
#include <cstdio>

namespace Luna {
	Utilities* Util;
	int Secret, Guess;

	Utilities::Utilities() {
		srand(time(NULL));
		Secret = rand() % 10 + 1;
		Util = new Utilities();
	}

	int Utilities::NumberGame() {
		do {
			printf("Guess the number ;)");
			void(scanf("%d", &Guess));
			if (Secret < Guess) puts("The secret number is lower!");
			else if (Secret > Guess) puts("The secret number is higher!");
		} while (Secret != Guess);

		puts("Congrats, you finished the game.");
		return EXIT;
	}

	Utilities::~Utilities() {
		delete Util;
	}
}