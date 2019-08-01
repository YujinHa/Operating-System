#include "pthread.h"
#include "stdio.h"
#include "unistd.h"
#include "string.h"

#define SYNC __sync_synchronize()
#define PROCESS_NUMBER 7

volatile int number[PROCESS_NUMBER];
volatile bool choosing[PROCESS_NUMBER];
volatile int resource;

void lamport(int i) 
{
	bool check = false;
	choosing[i] = true;
	SYNC;
	int max = 0;
	for (int k = 0; k < PROCESS_NUMBER; ++k)
		max = number[k] > max ? number[k] : max;
	number[i] = max + 1;
	SYNC;
	choosing[i] = false;
	SYNC;
	for (int j = 0; j < PROCESS_NUMBER; ++j) 
	{
		check = false;
		while (choosing[j])
			printf("다른 프로세스가 숫자 가질 때까지 기다림\n");
		SYNC;
		while (number[j] != 0 && (number[j] < number[i] ||(number[j] == number[i] && j < i)))
			check = true;
		
		if(check == true)
			printf("%d번 프로세스가 CS에 들어가려고 했지만, %d번 프로세스가 들어가 있었음.\n", i, j);
	}
	if (resource != 0) 
		printf("리소스가 %d번 프로세스에 의해 얻어졌지만, %d번 프로세스가 사용중!\n", i, resource);

	resource = i;
	printf("%d번 프로세스가 CS들어가 있음.\n", i);
	SYNC;
	sleep(2);
	resource = 0;

	SYNC;
	number[i] = 0;
}

void *thread_body(void *a) 
{
	long thread = (long)a;
	lamport(thread);
	return NULL;
}

int main(int argc, char **argv) 
{
	memset((void*)number, 0, sizeof(number));
	memset((void*)choosing, 0, sizeof(choosing));
	resource = 0;

	pthread_t threads[PROCESS_NUMBER];

	for (int i = 0; i < PROCESS_NUMBER; ++i)
		pthread_create(&threads[i], NULL, &thread_body, (void*)((long)i));

	for (int i = 0; i < PROCESS_NUMBER; ++i)
		pthread_join(threads[i], NULL);

	return 0;
}
