# -*- mode: ruby -*-
# vi: set ft=ruby :

Vagrant.configure("2") do |config|
  config.vm.box = "generic/ubuntu1804"
  config.vm.hostname = "abilityedge#{rand(1000..9999)}.box"
  # config.vm.box_check_update = false
  # NOTE: This will enable public access to the opened port
  # config.vm.network "forwarded_port", guest: 80, host: 8080
  # config.vm.network "forwarded_port", guest: 80, host: 8080, host_ip: "127.0.0.1"

  # Create a private network, which allows host-only access to the machine
  # using a specific IP.
  # config.vm.network "private_network", ip: "192.168.33.10"

  # Create a public network, which generally matched to bridged network.
  # Bridged networks make the machine appear as another physical device on
  # your network.
  # config.vm.network "public_network"

  # Share an additional folder to the guest VM. The first argument is
  # the path on the host to the actual folder. The second argument is
  # the path on the guest to mount the folder. And the optional third
  # argument is a set of non-required options.
  # config.vm.synced_folder "../data", "/vagrant_data"
  #config.vm.synced_folder ".", "/vagrant"
  
  #config.vm.provider :libvirt do |libvirt|
  #  libvirt.cpus = 2
  #  libvirt.memory = 1024
  #  libvirt.nested = true
  #end

  # To run this box on a virtualbox, uncomment the following line and comment previous section
  config.vm.provider "virtualbox"
  config.vm.provider "hyperv"
  
  # View the documentation for the provider you are using for more
  # information on available options.

  config.vm.provision "file", source: "files/ability", destination: "ability"
  config.vm.provision "file", source: "files/ability-install.sh", destination: "ability-install.sh"
  config.vm.provision "shell", path: "files/bootstrap.sh"
end
