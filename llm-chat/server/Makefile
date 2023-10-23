#!/usr/bin/env make -f
# GamePrompt's Gandalf Builder
# Author: Sebastian Weigand <tdg@google.com>, 2023

# Note: This requires use of `docker buildx`. Ensure you are using an official Docker CE package.
# See: https://docs.docker.com/engine/install/ and https://docs.docker.com/build/architecture/
# for more information.

# Note: If building locally on an ARM-based (Apple-Silicon) Mac, you will need to disable
# containerd in your Docker configuration for `docker buildx` to function properly for
# non-ARM architectures as of Docker Engine version 24.0.2.

# Program and container names:
PROGRAM_NAME=gameprompt-server
ARCH=amd64
PLATFORM=linux
BINARY_NAME=$(PROGRAM_NAME)-$(PLATFORM)-$(ARCH)-static
BINARY_NAME_COMPRESSED=$(BINARY_NAME)-upx

# Container and registry tags / metadata:
REGISTRY=us-docker.pkg.dev/gss-arena/gameprompt
TAG="$(PLATFORM)-$(ARCH)"

.PHONY : container push-container

all : binary compressed container push-container

# Compiled targets:
binary : $(BINARY_NAME)

# Compressed target:
compressed : $(BINARY_NAME_COMPRESSED)

# Containers:
container : $(BINARY_NAME_COMPRESSED)
	docker buildx build --platform=$(PLATFORM)/$(ARCH) -t $(REGISTRY)/$(PROGRAM_NAME):$(TAG) .

# Push to Artifact Registry:
push-container :
	docker push $(REGISTRY)/$(PROGRAM_NAME):$(TAG)

# Sanitation:
clean :
	rm -f $(BINARY_NAME)
	rm -f $(BINARY_NAME_COMPRESSED)

# Dependencies:
$(BINARY_NAME) : export CGO_ENABLED=0
$(BINARY_NAME) : export GOOS=$(PLATFORM)
$(BINARY_NAME) : export GOARCH=$(ARCH)
$(BINARY_NAME) :
	go build -o $(BINARY_NAME) $(PROGRAM_NAME).go

$(BINARY_NAME_COMPRESSED) : $(BINARY_NAME)
	upx -9 -o $(BINARY_NAME_COMPRESSED) $(BINARY_NAME)
