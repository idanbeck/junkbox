#include <stdio.h>

alignas (8) struct mystruct {
		char a;
		char b;
		alignas(4) char c[3];
};

int main(int argc, char *argv[]) {
	struct mystruct t;
	printf("sizeof mystruct: %d sizeof t: %d\n", sizeof(struct mystruct), sizeof(t));
	printf("sizeof t.c: %d\n", sizeof(t.c));

	return 0;
}
