
all:
	make dependencies
	make core

dependencies: ;

core:
	dotnet restore
	dotnet build -c=Release -o build

clean:
	rm -rf build/ obj/ packages/ bin/
